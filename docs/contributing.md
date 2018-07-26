## Running Integration Tests

Integration tests in the project `MSBLOC.Core.IntegrationTests` require the following:
- Setup on GitHub.com
  - A personal fork of [justaprogrammer/TestConsoleApp1](https://github.com/justaprogrammer/TestConsoleApp1)
  - A [GitHub App](https://developer.github.com/apps/) with Read & Write Access to Checks
  - The GitHub App needs to be integrated with the personal fork
- Environment variables set locally
  - **MSBLOC_INTEGRATION_APP_OWNER**: Set to the owner of the fork of `TestConsoleApp1`
  - **MSBLOC_INTEGRATION_APP_NAME**: Set to the name of the fork of `TestConsoleApp1`
  - **MSBLOC_GITHUB_APPID**: Set to the AppId of the GitHub App
  - **MSBLOC_GITHUB_KEY**: Set to the contents of a private key created for the GitHub App