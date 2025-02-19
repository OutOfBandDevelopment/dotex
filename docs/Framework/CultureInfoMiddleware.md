# OoBDev - CultureInfo Middleware

## Summary

This middle ware enables the exchange of language and culture information between the 
client and the server applications.

To use this middleware in your component add the `ICultureInfoAccessor` so your constructor.

The client should send the `Accept-Language` header on all requests, especially those that 
require server side language selection.  The server will require the configured culture on
the `Content-Language` header.

## References and Notes

* https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Accept-Language
* https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Content-Language
