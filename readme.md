# Frickr

## Summary

Unwrap Flickr data dump to album folder structure with embedded EXIF metdata.

## Features

In Flickr takeout zip archives, the photo metadata is stored in separate json files. 
This tool inject the metadata from json into the actual images as EXIF properties.

## Build

Written in C# with [dotnet 8](https://dotnet.microsoft.com/download/dotnet) it should run on Linux, macOS, Windows.  

Clone the repository and compile the source with:
```cmd
dotnet build
```
## Receipt

1. Request your data from Flickr account settings. 
2. Wait for Flickr to prepare the data dump for you.
3. Download all the files to a local folder.
4. Extract all the zip files to a single folder.
5. Run the tool (if you omit the extraction folder parameter, it will default to the current folder).
```cmd
dotnet run {extraction folder}
```

## Bugs?

Please feel free to submit pull request if you find a bug.

## Legal notice

This repository or its author is in no way affiliated with Flickr.
Make sure to create safe backups of your data files before running Frickr. 
See also [MIT License](LICENSE).
