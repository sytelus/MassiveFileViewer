# MassiveFileViewer
This tool will allow you to browse very large files. It simply does this by loading only few portion of the file at a time. It allows you to browse using previous/next or jump to arbitrary pages. It also allows you to search in large files.

##Goals
- Always be responsive
- Very fast
- Ability to load files where each row is XML, JSON or other formats
- Ability to do sophisticated queries, preferably using LINQ syntax

So far XML format and some query syntax is implemented. My major focus had been to implement various concurrent file I/O using new async wait. Next step would be to implement query language and JSON format.
