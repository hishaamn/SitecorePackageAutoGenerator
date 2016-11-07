# SitecorePackageAutoGenerator
Repository for the module generating package throught .csv

All you need is to create a CSV file and have only 3 columns.

- Column 1
Item or file path.

    Example

Item Path: /sitecore/content/New Sample Item

File Path: /layouts/Sample Datasource Sublayout.ascx

- Column 2
You need to specify the type. It is either Item or File.

- Column 3
It is used to specify whether to include sub items. This is used for both Items and files, so you need to specify it. It is either True or False.

Once the CSV file is available, you can use the tool to upload the file and it will generate the package based on the path provided.

This tool makes deployment easier. For example, you can have a single sheet on Excel which all the developers paste the path of the files or items, then using this only, you can generate the package.

Note that it is making use of the Add Static items/files
