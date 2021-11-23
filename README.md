# entity-cache (WIP)

this repository provides a local cache of a remote database with support for
* resetting changes in the local cache
* automatically detecting changes in locally cached entities
* pushing local changes to the remote database 
* pulling remote changes into the local cache
* synchronizing the local cache with the remote database
* solving conflicts caused by concurrent changes
* cascading pushes and pulls for referenced entities

the cache is designed for use with entity framework.
