# API Documentation

## Home Module

### `GET /`

Returns a string message.

**Response:**

```
"You're 59+20, 73!"
```

### `GET /refreshSecret`

Generates a new secret key for a user.

**Parameters:**

- `userId`: The user's ID.
- `sessionId`: The user's session ID.

**Response:**

- `statusCode`: 0 for success, -1 for failure.
- `message`: "Success" or error message.
- `data`: Contains the new secret key.

### `GET /getRandomName`

Generates a random name.

**Response:**

- `statusCode`: 0 for success, -1 for failure.
- `message`: "Success" or error message.
- `data`: Contains the generated name.

## Queue Module

### `GET /queue/current`

Gets the current queue.

**Parameters:**

- `showPassed`: 1 to show passed, 0 to not show.
- `qth`: Queue ID.

**Response:**

- `statusCode`: 0 for success, -1 for failure.
- `message`: "Success" or error message.
- `data`: Contains the queue documents.

### `GET /queue/insert`

Inserts a user into the queue.

**Parameters:**

- `qth`: Queue ID.
- `userId`: The user's ID.
- `isRight`: Integer value.

**Response:**

- `statusCode`: 0 for success, -1 for failure.
- `message`: "Success" or error message.
- `data`: Contains the queue ID.

### `GET /queue/pass`

Passes a user in the queue.

**Parameters:**

- `qth`: Queue ID.
- `userId`: The user's ID.
- `queueId`: The queue's ID.

**Response:**

- `statusCode`: 0 for success, -1 for failure.
- `message`: "Success" or error message.