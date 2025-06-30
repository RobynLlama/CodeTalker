# Changelog

## Version 1.1.4

- Adds a config file option for Packet Debugging. This is meant for developers to see all traffic and debug packet issues. It is `FALSE` by default. Please only enable if you need it, because it is likely slow

## Version 1.1.3

- Exception logging will ignore type load errors for the sake of debugging. This is a minor change aimed at developers

## Version 1.1.2

- Adds much smarter exception handling to help users understand where networking errors occur and what mod is to blame
- Lowers network buffer to 4kb from 10kb for better memory usage
