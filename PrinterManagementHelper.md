# Printer Management Helper Notes

## Features

Search for printers by name
View printer information
Change printer properties (Name, IP, Port) in bulk
Produce "Change File" that contains user's desired changed
Read and act on "Change File"
Testing connectivity to printer ("replacing" telnet)

## User Stories

As an Employee I want to Search For Customer's Printers Easily so that I can Validate The Printer Settings
Given the employee has started the program, when they fill in the search box, then the employee should see printers matching their criteria

As an Employee I want to Change Multiple Printers At Once so that I can Spend Less Time Going Through Menus
Given the employee has selected printers they want to change, when they modify the values and confirm the changes, then the employee should see what changes went through and which did not

As an Employee I want to Save The Changes I'm Making To A File so that I can Distribute The Changes To My Other Servers.
Given the employee has modified the values they intend to, when they press the save button, then a file should be made that can be parsed by the program

As an Employee I want to Load Changes Setup From Another Server so that I Don't Spend Time Repeating My Work
Given the employee has a change file from the program, when they choose to load the file, then the program should prepopulate with printers and their desired changes

## Data Models

TCP/IP Printer
  Printer Name
  IP Address
  Port Number
  
