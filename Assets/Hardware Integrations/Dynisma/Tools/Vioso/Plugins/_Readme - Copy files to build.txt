The calibration files are NOT copied by default to the build.

The file BuildPostProcessor.cs in Assets\Hardware Integrations\Dynisma\Tools\Vioso\Editor can automatically copy the files to the build.

For that, either edit and uncomment the #if/#endif region in the file, or include the compilation define _DYNISMA_DMG1 in the project.

NOTE: If enabled, this script ALWAYS copies the callibration files to ALL builds!! Ensure to enable it only for Dynisma DMG1 builds.