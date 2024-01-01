# chess2

The chess2 website frontend

## Installation

First, install the dependencies:

```bash
  npm install
```

then run the dev server:

```bash
  npm run dev
```

Open http://localhost:3000 in your browser to view the website.

## OpenAPI SDK

The backend of this project is made in FastAPI. FastAPI automatically generates openapi specs. Using this, we can automatically generate a typesafe SDK.

This project uses the openapi-generator package. To generate the client, run

```bash
  npm run generate-client
```

The SDK will be generated into `src/client`.

## Features

To view the features, check out the [backend documentation](https://github.com/YishaiYosifov/chess2-backend#features).

## Screenshots

![Signup Page](https://github.com/YishaiYosifov/chess2-frontend/assets/74960133/f352b93f-f6af-4f0b-ab7b-573b71c84f82)

![Profile Page](https://github.com/YishaiYosifov/chess2-frontend/assets/74960133/1e50a2f7-1ae1-48dc-b534-568a5d1acb17)

![Play Page](https://github.com/YishaiYosifov/chess2-frontend/assets/74960133/1a5f6eba-7300-4977-a6c4-a125b4a5e44f)

## Testing

To run tests, run the following command:

```bash
  npm run test
```

To run the test watcher, run the following command:

```bash
  npm run test:watch
```

Tests are located in the `__tests__` subdirectory in each directory. Test are made with [React Testing Library](https://github.com/testing-library/react-testing-library) and [vitest](https://github.com/vitest-dev/vitest).
