# GetTheText
xgettext alternative for C# using the Roslyn analyzer  
GetTheText is used for extracting translation strings from C# files

## How to use
GetTheText will search for methods, which are used for translating strings, e.g. `Tr(string)` in Godot, or `_(string, object[])`, custom methods will be supported soon, if it finds one, it will check if the first argument is a string. Support for attributes will be added too.  
Only string literals (`"text"`) and verbatim string literals (`@"text"`) are supported, interpolated strings (`$"a = {a}"`) will never be supported.  
Output is printed to stdout, so it must be redirected to a file, e.g. `./GetTheText Program.cs > en.pot`  
The following file
```cs
// Program.cs
Console.WriteLine(_("Hello, World!"));
// !!!
// Top-level statements are not supported by Roslyn analyzer yet
// therefore GetTheText will not work with them, this is just an example
```
Will produce the following output
```
# en.pot
# Program.cs(1,18)
msgid "Hello, World!"
msgstr ""
```

## Why GetTheText?
xgettext extracts **ALL** the strings in a file, not just strings, which are used in a method that's used for translating as the first parameter

## Roadmap
* [ ] Command line options 
* [ ] Add option for scanning directories
* [x] Add option for extracting strings from attibutes
* [ ] Add option for custom translation functions and attributes