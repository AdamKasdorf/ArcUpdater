ArcUpdater is a command-line tool to check for updates to ArcDPS, download the latest version, verify the integrity of the assembly, and install to the current working directory or specified directories and file paths.

If executed with no optional command-line arguments, runs a standard check and install/update to the current working directory.
Accepts multiple path arguments, and supports environment variable expansion and paths relative to the current working directory.
Moves existing assemblies to the Recycle Bin by default rather than deleting/overwriting. Can be overriden with the --del option.

Use command --help or -? for complete help information.
