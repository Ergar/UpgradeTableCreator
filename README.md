# UpgradeTableCreator

Allows to copy existings table fields into a new table. The application outputs a text file that can be imported via the DevClient.

## Usage
Following commands can be used when calling the .exe:

Usage example: server=localhost database=localdb

### [Server] (required) 
  Server adress to the database
### [Database] (required): Database name
### [FromTableId] (default 0, optional)
### [ToTableId] (default 0, optional)
### [FromFieldId] (default 0, optional)
### [ToFieldId] (default 0, optional)
### [StartNewTableId] (default 0, optional)
  The starting id for the new created tables
### [TablePrefix] (default "UPG ", optional)
  Prefixes the table name. Skips the renaming if it will exceed 30 characters. 
### [FieldPrefix] (default "", optional)
  Prefixes the field name. Skips the renaming if it will exceed 30 characters. 
### [VersionList]
  The new version list the table will have.
