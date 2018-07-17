Import-Module (Join-Path $PSScriptRoot '..\MSBuildLogOctokitChecker.psd1') -Force
Describe 'Test Get-OctoKitMsbuildLogBaseUrl and Get-OctoKitMsbuildLogBaseUrl' {
    It 'Has the proper default url' {
        Get-OctoKitMsbuildLogBaseUrl | Should Be 'http://localhost:64952'
    }
    It 'Setter mutates getter' {
        Set-OctoKitMsbuildLogBaseUrl 'http://new'
        Get-OctoKitMsbuildLogBaseUrl | Should Be 'http://new'
    }
    It 'Respects Passthru' {
        Set-OctoKitMsbuildLogBaseUrl 'http://nothing' | Should BeNullOrEmpty
        Set-OctoKitMsbuildLogBaseUrl 'http://something' -Passthru | Should Be 'http://something'
    }
}