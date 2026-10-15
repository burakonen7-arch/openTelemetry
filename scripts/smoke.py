"""Run against a migrated, running API: python scripts/smoke.py [base_url]."""
import json, sys, urllib.request, urllib.error
base = (sys.argv[1] if len(sys.argv) > 1 else 'http://127.0.0.1:8080') + '/api/products'
def request(method, path='', data=None):
    req = urllib.request.Request(base + path, data=json.dumps(data).encode() if data is not None else None,
                                 headers={'Content-Type': 'application/json'}, method=method)
    try: response = urllib.request.urlopen(req)
    except urllib.error.HTTPError as error: response = error
    raw = response.read()
    return response.status, json.loads(raw) if raw else None
status, product = request('POST', data={'name':'Telemetry keyboard','price':99.95,'stock':5})
assert status == 201, (status, product)
path = '/' + product['id']
try:
    status, found = request('GET', path)
    assert status == 200 and found['name'] == 'Telemetry keyboard'
    status, updated = request('PUT', path, {'name':'Updated keyboard','price':119.95,'stock':3})
    assert status == 200 and updated['stock'] == 3
    status, items = request('GET')
    assert status == 200 and any(x['id'] == product['id'] for x in items)
    assert request('POST', data={'name':'','price':-1,'stock':-1})[0] == 400
    assert request('GET', '?page=0')[0] == 400
    assert request('GET', '/00000000-0000-0000-0000-000000000000')[0] == 404
finally:
    assert request('DELETE', path)[0] == 204
assert request('GET', path)[0] == 404
print('PASS: create, read, update, list, validation, pagination, missing product, delete (9 checks)')
