# ScaleOut Object Report Generator

The ScaleOut Object Report Generator is a command line utility for Windows that generates an HTML report containing memory usage statistics for each object namespace in the ScaleOut service. This project is intended as a starting point for developers who may want to do more detailed reporting on their object populations.

A "namespace" corresponds to any of the following concepts:

 - A named cache that was defined through the NamedCache API.
 - An ASP.NET web application that stores its session state in ScaleOut StateServer. 
 - An application name that was defined through the legacy CachedDataAccessor API. 

## Usage

Running the executable from the command line will produce an html file in the current directory whose name corresponds to the current, local system time (i.e, `20170109-130948.html`). 

Use the **-m** flag to perform multithreaded metadata retrieval to reduce run times on large object stores. Note, however, that multithreaded retrieval will put more load on the ScaleOut service.

## Prerequisties

 - .NET 4.0 or higher.
 - ScaleOut StateServer (the utility may be run directly on a ScaleOut host or on a system with ScaleOut's remote client libraries installed).

## Status

Alpha (initial development release).

## License

Apache 2.0