# OoBDev - Blob Storage Design

## Summary

Provide a common means to access blob containers.

## Notes

* Provide a way to have containers defined per point of use.  
  * Need a generic interface to allow for defining multiple container providers
  * Need the ability to configurably select between blob providers
  * Need an attribute to tag container name based on generic key