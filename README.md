# Printer Helper

## Overview

A small application being built to help easily manage basic information about networked printers.

A pet project of a tool to potentially use at work if I can get the functionality where I need it.

On a small scale, the business logic works fine, but when you have 75+ TCP/IP Printers the weight of WMI Queries add up fast.

## To-Do

[X] Implement the ability to change Printer Name, IP, Port, and PortName
[ ] Lazy Loading Printer Properties (WMI Queries are heavy)
[ ] Add Async/Multithreaded logic
[ ] Implement UI

### Features

[ ] Search for printers by name
[ ] View printer information
[ ] Change printer properties (Name, IP, Port) in bulk
[ ] Produce "Change File" that contains user's desired changed
[ ] Read and act on "Change File"
[ ] Testing connectivity to printer