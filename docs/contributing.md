# Contributing

## Initial Setup
1. Create a GitHub app

## Running Web Application
1. 

## Running Integration Tests

Integration tests in the project `.Core.IntegrationTests` require the following:
- Setup on GitHub.com
  - A personal access token (public access, no permissions needed)
  - A personal fork of [justaprogrammer/TestConsoleApp1](https://github.com/justaprogrammer/TestConsoleApp1)
  - A [GitHub App](https://developer.github.com/apps/) with Read & Write Access to Checks
  - The GitHub App needs to be installed to the personal fork
- Environment variables set locally
  - **BCC_INTEGRATION_APP_OWNER**: Set to the owner of the fork of `TestConsoleApp1`
  - **BCC_INTEGRATION_APP_REPO**: Set to the name of the fork of `TestConsoleApp1`
  - **BCC_GITHUB_APPID**: Set to the AppId of the GitHub App
  - **BCC_INTEGRATION_GITHUB_KEY**: Set to the contents of a private key created for the GitHub App
  - **BCC_INTEGRATION_APP_INSTALLATION_ID**: Set to the installation ID of the GitHub App to TestConsoleApp1
  - **BCC_INTEGRATION_TOKEN**: Set to the personal access token created
  - **BCC_INTEGRATION_USERNAME**: Set to the username of the personal access token created
