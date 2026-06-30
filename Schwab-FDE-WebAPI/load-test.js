// Save as: load-test.js and run with k6 (https://k6.io/docs/getting-started/installation/)
// Usage: k6 run load-test.js

import http from 'k6/http';
import { check, sleep } from 'k6';
import { Rate, Trend, Counter, Gauge } from 'k6/metrics';

// Custom metrics
const errorRate = new Rate('errors');
const responseTime = new Trend('http_req_duration');
const cacheHitCount = new Counter('cache_hits');
const currentVUs = new Gauge('vus');

// Test configuration
export const options = {
  stages: [
    { duration: '30s', target: 10 },  // Ramp-up: 10 VUs
    { duration: '1m', target: 50 },   // Stay: 50 VUs
    { duration: '1m', target: 100 },  // Peak: 100 VUs
    { duration: '30s', target: 0 },   // Ramp-down
  ],
  thresholds: {
    'http_req_duration': ['p(95)<200', 'p(99)<500'],
    'http_req_failed': ['rate<0.01'],
    'errors': ['rate<0.05'],
  },
};

// Test data
const BASE_URL = 'https://localhost:7001';

function generateDailyReturnRequest(index) {
  return {
    portfolioId: `PF-${1000 + index}`,
    valuationDate: '2026-06-14',
    beginMarketValue: 1000000 + (index * 1000),
    endMarketValue: 1035000 + (index * 1000),
    netCashFlow: 10000 + (index * 100),
    benchMarketReturnPct: 1.8,
    currency: 'USD',
    requestedBy: `advisor-${index % 10}`,
  };
}

function generateAttributionRequest(index) {
  return {
    requestId: `ATTR-${1000 + index}`,
    portifolioId: `PF-${2000 + index}`,
    valuationdate: '2026-06-14',
    groups: [
      {
        groupName: 'Equity',
        weightPct: 60,
        returnPct: 1.5,
      },
      {
        groupName: 'FixedIncome',
        weightPct: 30,
        returnPct: 0.4,
      },
      {
        groupName: 'Cash',
        weightPct: 10,
        returnPct: 0.05,
      },
    ],
    currency: 'USD',
    requestedBy: `advisor-${index % 10}`,
  };
}

function generateAttributionRequestWithFallback(index) {
  return {
    requestId: `ATTR-FB-${1000 + index}`,
    portifolioId: `PF-${2000 + index}`,
    valuationdate: '2026-06-14',
    groups: [
      {
        groupName: 'Equity',
        weightPct: 60,
        returnPct: 1.5,
      },
      {
        groupName: 'FixedIncome',
        weightPct: 30,
        returnPct: null,
        fallbackReturnedPct: 0.4,
      },
      {
        groupName: 'Cash',
        weightPct: 10,
        returnPct: 0.05,
      },
    ],
    currency: 'USD',
    requestedBy: `advisor-${index % 10}`,
  };
}

export default function () {
  const index = Math.floor(__VU * __ITER);
  currentVUs.value = __VU;

  // Test 1: Daily Return Endpoint (70% of traffic)
  if (Math.random() < 0.7) {
    testDailyReturn(index);
  }
  // Test 2: Attribution Endpoint with all returns (20% of traffic)
  else if (Math.random() < 0.67) {
    testAttributionValid(index);
  }
  // Test 3: Attribution with fallback (10% of traffic)
  else {
    testAttributionFallback(index);
  }

  sleep(1);
}

function testDailyReturn(index) {
  const payload = JSON.stringify(generateDailyReturnRequest(index));

  const params = {
    headers: {
      'Content-Type': 'application/json',
    },
    tags: { name: 'DailyReturn' },
  };

  const response = http.post(`${BASE_URL}/api/performance/daily-return`, payload, params);

  const success = check(response, {
    'DailyReturn: status is 200': (r) => r.status === 200,
    'DailyReturn: response time < 100ms': (r) => r.timings.duration < 100,
    'DailyReturn: has portfolio return': (r) => r.json('portfolioReturnPct') !== null,
    'DailyReturn: has status': (r) => ['valid', 'review_required', 'invalid_input'].includes(r.json('status')),
  });

  errorRate.add(!success);
  responseTime.add(response.timings.duration);

  if (response.status === 200 && response.timings.duration < 5) {
    cacheHitCount.add(1);
  }
}

function testAttributionValid(index) {
  const payload = JSON.stringify(generateAttributionRequest(index));

  const params = {
    headers: {
      'Content-Type': 'application/json',
    },
    tags: { name: 'AttributionValid' },
  };

  const response = http.post(`${BASE_URL}/api/performance/attribution`, payload, params);

  const success = check(response, {
    'Attribution: status is 200': (r) => r.status === 200,
    'Attribution: response time < 100ms': (r) => r.timings.duration < 100,
    'Attribution: has total contribution': (r) => r.json('totalContributionPct') !== null,
    'Attribution: status is VALID': (r) => r.json('status') === 'VALID',
    'Attribution: not degraded': (r) => r.json('degraded') === false,
  });

  errorRate.add(!success);
  responseTime.add(response.timings.duration);

  if (response.status === 200 && response.timings.duration < 5) {
    cacheHitCount.add(1);
  }
}

function testAttributionFallback(index) {
  const payload = JSON.stringify(generateAttributionRequestWithFallback(index));

  const params = {
    headers: {
      'Content-Type': 'application/json',
    },
    tags: { name: 'AttributionFallback' },
  };

  const response = http.post(`${BASE_URL}/api/performance/attribution`, payload, params);

  const success = check(response, {
    'AttributionFallback: status is 200': (r) => r.status === 200,
    'AttributionFallback: response time < 100ms': (r) => r.timings.duration < 100,
    'AttributionFallback: has warnings': (r) => r.json('warnings').length > 0,
    'AttributionFallback: fallback used': (r) => JSON.stringify(r.json('groupContributions')).includes('FALLBACK_USED'),
  });

  errorRate.add(!success);
  responseTime.add(response.timings.duration);

  if (response.status === 200 && response.timings.duration < 5) {
    cacheHitCount.add(1);
  }
}

// Separate test scenario: Idempotency stress test
export function idempotencyTest() {
  const options = {
    stages: [
      { duration: '30s', target: 1 },
    ],
  };

  // Make 1000 identical requests (should all hit cache after first)
  for (let i = 0; i < 100; i++) {
    const payload = JSON.stringify({
      requestId: 'IDEMPOTENT-TEST',
      portifolioId: 'PF-IDEMPOTENT',
      valuationdate: '2026-06-14',
      groups: [
        { groupName: 'Equity', weightPct: 60, returnPct: 1.5 },
        { groupName: 'FixedIncome', weightPct: 30, returnPct: 0.4 },
        { groupName: 'Cash', weightPct: 10, returnPct: 0.05 },
      ],
      currency: 'USD',
      requestedBy: 'advisor01',
    });

    const response = http.post(`${BASE_URL}/api/performance/attribution`, payload, {
      headers: { 'Content-Type': 'application/json' },
    });

    check(response, {
      'Idempotent: fast response (<5ms)': (r) => r.timings.duration < 5,
      'Idempotent: status 200': (r) => r.status === 200,
    });

    if (response.timings.duration < 5 && i > 0) {
      cacheHitCount.add(1);
    }
  }
}

// Performance benchmark
export function benchmarkTest() {
  console.log('Starting performance benchmark...');

  const tests = [
    {
      name: 'DailyReturn',
      url: `${BASE_URL}/api/performance/daily-return`,
      payload: generateDailyReturnRequest(1),
      iterations: 100,
    },
    {
      name: 'Attribution',
      url: `${BASE_URL}/api/performance/attribution`,
      payload: generateAttributionRequest(1),
      iterations: 100,
    },
  ];

  for (const test of tests) {
    const times = [];

    for (let i = 0; i < test.iterations; i++) {
      const response = http.post(test.url, JSON.stringify(test.payload), {
        headers: { 'Content-Type': 'application/json' },
      });
      times.push(response.timings.duration);
    }

    const sorted = times.sort((a, b) => a - b);
    const avg = times.reduce((a, b) => a + b) / times.length;
    const p50 = sorted[Math.floor(sorted.length * 0.5)];
    const p95 = sorted[Math.floor(sorted.length * 0.95)];
    const p99 = sorted[Math.floor(sorted.length * 0.99)];

    console.log(`\n${test.name} Performance (${test.iterations} iterations):`);
    console.log(`  Average: ${avg.toFixed(2)}ms`);
    console.log(`  P50: ${p50.toFixed(2)}ms`);
    console.log(`  P95: ${p95.toFixed(2)}ms`);
    console.log(`  P99: ${p99.toFixed(2)}ms`);
    console.log(`  Min: ${sorted[0].toFixed(2)}ms`);
    console.log(`  Max: ${sorted[sorted.length - 1].toFixed(2)}ms`);
  }
}
