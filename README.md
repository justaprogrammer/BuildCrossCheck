# MSBuildLogOctokitChecker

<a href="https://ci.appveyor.com/project/JustAProgrammer/msbuildlogoctokitchecker">
  <img alt="AppVeyor badge" target="_blank"
       src="https://ci.appveyor.com/api/projects/status/github/justaprogrammer/MSBuildLogOctokitChecker?svg=true&branch=master" />
</a>

## Overview

MSBuildLogOctokitChecker is a GitHub App that uses MSBuild Binary log files to help check pull requests. Integrate it with the build process of any repository that uses MSBuild, and it will identify build warnings and errors in Pull Requests that you open like this:

<img src="./docs/images/testconsole1-warning-pr-changes.png">
<img src="./docs/images/testconsole1-warning-pr-check-runs.png">

_(see examples at: [justaprogrammer/TestConsoleApp1/pulls](https://github.com/justaprogrammer/TestConsoleApp1/pulls))_

## Getting started

The authors of MSBuildLogOctokitChecker maintain a hosted version of the source code you see here.
Install it by adding the GitHub App: [github.com/apps/MSBuildLogOctokitChecker](https://github.com/apps/MSBuildLogOctokitChecker) to your repositories and following the [integration documentation](docs/integrating.md). 

## Documentation

You can find our documentation [here](docs/readme.md).

## Licenses
- This source is distributed under under the AGPL.
- The Powershell module is distributed under the MIT License.

## More information
- [MSBuild Log](http://msbuildlog.com/)
- [GitHub Checks API](https://developer.github.com/v3/checks/)

## Questions?

Please [file an issue](https://github.com/justaprogrammer/MSBuildLogOctokitChecker/issues/new/choose)! If you'd prefer to reach out in private, please send an email to Stanley.Goldman@gmail.com.
