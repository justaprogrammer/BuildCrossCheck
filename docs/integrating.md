# Integrating

## Adding the GitHub App

Add the **Build Cross Check** [GitHub App](https://github.com/apps/build-cross-check) to the repositories that you 
would like to work with.

## Create a token

In order to verify your build server, create a token to use when sending build logs.
After installing the application to your repo, head to **Build Cross Check** [web app](https://buildcrosscheck.azurewebsites.net/) 
and login with your GitHub account. There you will be able create a token per repository as well as
revoke prior tokens if necessary.

## Build Integration

This example is based on AppVeyor, but this will work from any CI system. For more information see the documentation for [BCC-MSBuildLog](https://github.com/justaprogrammer/BCC-MSBuildLog/blob/master/docs/usage.md).

1. Install the GitHub App on the desired GitHub Repositories or Organizations.

2. Generate a token using the steps [above](#create-a-token) and set a [secure environment variable](https://www.appveyor.com/docs/build-configuration/#secure-variables) named `BCC_TOKEN`.

3. Add the nuget package `BCC-MSBuild` to a project in your soution.

4. Integrate into your build by adding the msbuild logger:
   `msbuild [Solution] -logger:packages\BCC-MSBuildLog.1.0.0\tools\net472\BCCMSBuildLog.dll`

5. Enjoy your warnings and errors.