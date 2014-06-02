#!/bin/bash
echo "Starting script"

while :
do
	echo "Updating git repository"
	git pull
	echo "Compiling server project"
	xbuild /p:Configuration=Release TCPGameServer.csproj	
	echo "Starting server"
	./bin/Release/TCPGameServer.exe
done
echo "Quitting script"
