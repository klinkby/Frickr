# Frickr

## Summary
Unwrap Flickr data dump to album folder structure with embedded EXIF metdata.

## Features

Read flickr-generated zip archives directly extracting photos into album-subfolders, while injecting EXIF metadata like *description* and *keywords* set in Flickr. 

## Build

Written in C# with [dotnet core](https://dotnet.microsoft.com/download/dotnet-core) runtime it should run on Linux, macOS, Windows.  

Clone the repository, go to `/src`. To compile the source run:
```cmd
dotnet build
```


## Receipt

1. Request your data from Flickr account settings. 
2. Wait for Flickr to prepare the data dump for you.
3. Download all the files to a local folder.
4. Go to Run 
```cmd
dotnet run {sourceDir} {targetDir}
```

## Bugs?

Please feel free to submit pull request if you find a bug.

## Legal notice

This repository or its author is in no way affiliated with Flickr. Make sure to create safe backups of your data files before running Frickr. See also License section below.

## License

MIT License

Copyright (c) 2019 Mads Breusch Klinkby

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

