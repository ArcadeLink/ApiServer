# API 文档

## Home 模块

### `GET /`

返回一个测试字符串消息。

**响应:**

```
"You're 59+20, 73!"
```

### `GET /refreshSecret`

为用户生成新的密钥。

**参数:**

- `userId`: 用户的 ID。
- `sessionId`: 用户的会话 ID。

**响应:**

- `statusCode`: 0 表示成功，-1 表示失败。
- `message`: "Success" 或错误消息。
- `data`: 包含新的密钥。

### `GET /getRandomName`

生成一个随机名称。

**响应:**

- `statusCode`: 0 表示成功，-1 表示失败。
- `message`: "Success" 或错误消息。
- `data`: 包含生成的名称。

## Queue 模块

### `GET /queue/current`

获取当前队列。

**参数:**

- `showPassed`: 1 表示显示已通过，0 表示不显示。
- `qth`: 游戏厅的 ID（地理位置）。

**响应:**

- `statusCode`: 0 表示成功，-1 表示失败。
- `message`: "Success" 或错误消息。
- `data`: 包含队列文档。

### `GET /queue/insert`

将用户插入队列。

**参数:**

- `qth`: 游戏厅的 ID（地理位置）。
- `userId`: 用户的 ID。
- `isRight`: 插入左机或右机，当只有一个机器时，UI中仅显示一个机器，此参数指示排卡在左机还是右机。

**响应:**

- `statusCode`: 0 表示成功，-1 表示失败。
- `message`: "Success" 或错误消息。
- `data`: 包含队列 ID。

### `GET /queue/pass`

经过队列中的用户。

**参数:**

- `qth`: 游戏厅的 ID（地理位置）。
- `userId`: 用户的 ID。
- `queueId`: 队列的 ID。

**响应:**

- `statusCode`: 0 表示成功，-1 表示失败。
- `message`: "Success" 或错误消息。