# How to setup a new pipeline as a contributor.

The mod comes with two pipelines for each contributor, a Regular build and a Quick Build.

## Regular Build

The regular build does the following:

	1. Changes the Build mode to either Release or Debug
	2. Runs the "Complete Build" pipeline, which builds the mod DLL, Bundles, languages, etc.
	3. Copies the result to your path of choice.

The path of choice is what's called as [NAME]Path, you'll be able to change it's value once you start setting up your pipelines.

## Quick Build

As the name implies, the quick build does the following:

	1. Changes the build mod to either Release or Debug
	2. Runs the "Stage Assemblies" pipeline, which only builds the DLL, perfect if youre only doing code related changes.
	3. Copies the results to your path of choice.

The path of choice is what's called as [NAME]Path, you'll be able to change it's value once you start setting up your pipelines.

## Setting up your pipeline

1. Duplicate the "Nebby" folder, and rename it to your own name
2. Enter the folder
3. Rename all assets to begin with your name instead of "Nebby"
4. Click [YOURNAME]Path
5. Click on "Constant"
6. Replace the value with your R2ModMan profile