#!/bin/bash
wait_time=20s

echo creating resources in $wait_time  
sleep $wait_time  
echo starting...

echo 'creating database'  
/opt/mssql-tools/bin/sqlcmd -S localhost -U SA -P $SA_PASSWORD -I -Q "CREATE DATABASE BlazingPizza;"