# Authentication Guide

Simple JWT-based authentication for the Leaderboard API.

## Quick Start

1. **Start the API:**
   ```bash
   docker-compose up -d
   ```

2. **Generate a token:**
   ```bash
   USER_ID="player123"
   TIMESTAMP=$(date +%s000)
   SECRET="your-game-partner-secret"
   
   DATA_STRING="${USER_ID}:${TIMESTAMP}"
   SIGNATURE=$(echo -n "$DATA_STRING" | openssl dgst -sha256 -hmac "$SECRET" -binary | base64)
   
   curl -X POST http://localhost:8080/api/v1/auth/webhook/token \
     -H "Content-Type: application/json" \
     -d '{
       "userId": "'$USER_ID'",
       "timestamp": '$TIMESTAMP',
       "signature": "'$SIGNATURE'"
     }'
   ```

3. **Use the token:**
   ```bash
   curl -X GET http://localhost:8080/api/v1/Leaderboard/user/stats \
     -H "Authorization: Bearer YOUR_TOKEN_HERE"
   ```

## Environment Variables

Set these environment variables for authentication:

- `Leaderboard_SECRET_KEY`: JWT signing key (required)
- `Leaderboard_GAME_PARTNER_SECRET`: Secret for request verification (required)
- `Leaderboard_TOKEN_EXPIRATION_MINUTES`: Token expiration in minutes (default: 60)

## How It Works

1. **Token Request**: Send a signed request with userId, timestamp, and HMAC-SHA256 signature
2. **Token Response**: Receive a JWT token valid for 60 minutes (configurable)
3. **API Access**: Include the token in the Authorization header for all API calls

## Token Request Format

```json
{
  "userId": "player123",
  "timestamp": 1650000000000,
  "signature": "base64-encoded-hmac-signature"
}
```

## Token Response Format

```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": 1650006000
}
```

## Signature Generation

Create the signature using HMAC-SHA256:

1. Create data string: `{userId}:{timestamp}`
2. Sign with your `Leaderboard_GAME_PARTNER_SECRET`
3. Encode as Base64

**Example (bash):**
```bash
DATA_STRING="player123:1650000000000"
SIGNATURE=$(echo -n "$DATA_STRING" | openssl dgst -sha256 -hmac "$SECRET" -binary | base64)
```

**Example (Node.js):**
```javascript
const crypto = require('crypto');
const dataString = `${userId}:${timestamp}`;
const signature = crypto.createHmac('sha256', secret).update(dataString).digest('base64');
```

**Example (Python):**
```python
import hmac
import hashlib
import base64

data_string = f"{user_id}:{timestamp}"
signature = base64.b64encode(
    hmac.new(secret.encode(), data_string.encode(), hashlib.sha256).digest()
).decode()
```

## API Endpoints

### Get Token
```
POST /api/v1/auth/webhook/token
```

### Validate Token
```
GET /api/v1/auth/validate
```

## Error Responses

- **400 Bad Request**: Invalid request format
- **401 Unauthorized**: Invalid signature or expired token

## Security Notes

- Keep your `Leaderboard_GAME_PARTNER_SECRET` secure
- Use HTTPS in production
- Generate fresh timestamps for each request
- Tokens expire after 60 minutes by default