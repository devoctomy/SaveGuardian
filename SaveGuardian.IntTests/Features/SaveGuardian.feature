Feature: SaveGuardian

Basic functionality of SaveGuardian, when invoked from the command line.

@saveguardian
Scenario: SaveGuardian correctly backs up save file to correct location
	Given VersionFolder configured with name <Name> for path <Path> with filter <Filter> and grace period of <GracePeriodSeconds> seconds
	And VersionFolders configuration file created at <VersionFoldersConfigPath>
	When SaveGuardian is started
	And wait for 5 seconds
	And file created in <Path> with extension <NewFileExtension> with key <NewFile1>
	And wait for <WaitSeconds> seconds
	Then copy of <NewFile1> created in version folder <Name> backup path
	And SaveGuardian process is closed

	Examples:

	| VersionFoldersConfigPath | Name     | Path            | Filter | GracePeriodSeconds | WaitSeconds | NewFileExtension | NewFile1 |
	| Config/folders.json      | IntTest1 | SomeGame/Saves/ | *.sav  | 5                  | 10          | .sav             | Test1    |
