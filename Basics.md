# Directory Structure #

  * bin - all the files (with the exception of binaries created or linked to by compiling) that make up the user installation.

  * compLexity Demo Player - the demo player project.

  * hosted - hosted maps and update files.

  * installer - the NSIS installer script and associated files.

  * libraries - third party library binaries.

  * Update - the update program project. Handles downloading the update manifest, calculating the difference between manifest entries and files in the users' file system, and downloading the required files.

  * Update Manifest Generator - project for generating an update manifest.

# Compiling #

Visual C# 2008 SP1 is required.

Since the .NET Framework 3.0 is a minimum requirement, the Update project is set to target that version. The main project is set to target 3.5 SP1 Client Profile, although any changes should be able to be compiled if the project is changed to 3.0, with one exception - the TimeZoneInfo code in the Server Browser. This can be temporarily disabled by commenting out the two USE\_TIMEZONEINFO preprocessor defines in server browser\MainWindow.xaml.cs and server browser\server collection\Gotfrag.cs.

The working directory for the debug executable is set to the bin folder in the solution root. The working directory for the release executable is always the full executable path.

A preprocessor step involves copying release binaries into the root bin folder, for convenience of testing the release program.