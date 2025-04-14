# GitIssuer API

GitIssuer is a RESTful API that allows you to add, modify, and close issues in repositories hosted on various Git providers (e.g., GitHub, GitLab).

## Features

- Add a new issue to a specified repository
- Modify an existing issue
- Close an existing issue

---

## Endpoints

### Add Issue

**POST** `/api/issue/{gitProviderName}/{repositoryOwner}/{repositoryName}/add`

Adds a new issue to the specified repository.

#### Parameters

| Name               | Type   | Location   | Description                             |
|--------------------|--------|------------|-----------------------------------------|
| `gitProviderName`  | string | path       | Name of the Git provider (e.g., GitHub) |
| `repositoryOwner`  | string | path       | Owner of the repository                 |
| `repositoryName`   | string | path       | Name of the repository                  |
| `issue`            | body   | JSON       | Contains `title` and `description`      |

#### Request Body (JSON)

```json
{
  "title": "Bug: Incorrect UI",
  "description": "The UI breaks when clicking the login button."
}
```

#### Response

- `201 Created` on success  
- `400/500/503` on error with details

##### Example Success Response

```json
{
  "url": "https://github.com/username/repository/issues/123"
}
```

This response indicates that the issue has been successfully created. The `url` provides a direct link to the issue.

---

### Modify Issue

**PUT** `/api/issue/{gitProviderName}/{repositoryOwner}/{repositoryName}/{issueId}/modify`

Modifies an existing issue.

#### Parameters

| Name               | Type   | Location | Description                              |
|--------------------|--------|----------|------------------------------------------|
| `gitProviderName`  | string | path     | Name of the Git provider                 |
| `repositoryOwner`  | string | path     | Owner of the repository                  |
| `repositoryName`   | string | path     | Name of the repository                   |
| `issueId`          | int    | path     | ID of the issue to modify                |
| `issue`            | body   | JSON     | Contains updated `title` and `description` |

#### Request Body (JSON)

```json
{
  "title": "Bug: UI crash on login",
  "description": "Updated description with clearer steps to reproduce."
}
```

#### Response

- `200 OK` on success  
- `400/500/503` on error with details

##### Example Success Response

```json
{
  "url": "https://github.com/username/repository/issues/123"
}
```

This response indicates that the issue has been successfully modified. The `url` provides a direct link to the updated issue.

---

### Close Issue

**PATCH** `/api/issue/{gitProviderName}/{repositoryOwner}/{repositoryName}/{issueId}/close`

Closes an existing issue.

#### Parameters

| Name               | Type   | Location | Description                              |
|--------------------|--------|----------|------------------------------------------|
| `gitProviderName`  | string | path     | Name of the Git provider                 |
| `repositoryOwner`  | string | path     | Owner of the repository                  |
| `repositoryName`   | string | path     | Name of the repository                   |
| `issueId`          | int    | path     | ID of the issue to close                 |

#### Response

- `200 OK` on success  
- `400/500/503` on error with details

```json
{
  "url": "https://github.com/username/repository/issues/123"
}
```

This response indicates that the issue has been successfully closed. The `url` provides a direct link to the closed issue.

---

## Error Handling

All endpoints provide consistent error responses:

### Error Response Format

```json
{
  "error": "Error message",
  "details": "Optional technical details"
}
```

### Possible Status Codes

- `200 OK` – Successful update/close operation  
- `201 Created` – Successful issue creation  
- `400 Bad Request` – Unsupported Git provider  
- `500 Internal Server Error` – Unexpected errors  
- `503 Service Unavailable` – Failure in communication with Git provider

---