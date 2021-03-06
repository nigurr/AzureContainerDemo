#!/bin/bash

echo "***************************************** Docker Container Testing *****************************************"

echo ">>>>>>>>>> Cleaning up old files"
rm $PWD/SampleTestMap/Docker* $PWD/SampleTestMap/runtest*

echo ">>>>>>>>>> Building the docker generator"
dotnet build $PWD/TestMapParser/

echo ">>>>>>>>>> Running the docker generator"
dotnet run -p $PWD/TestMapParser/ -- -t 3 -c 1 -s $PWD/SampleTestMap -b .

echo ">>>>>>>>>> Generated docker files"
dockerfiles=$(find ./SampleTestMap -name 'Dockerfile*' | sed 's#.*/##')
echo "$dockerfiles"

#IFS=$'\n' array=($dockerfiles)
#for i in "${array[@]}"
#do
#	echo ">>>>>>>>>> Prepping and running docker $i"
#	docker run --rm -it $(docker build -f $PWD/SampleTestMap/$i $PWD/SampleTestMap -q)
#	echo $"\n"
#done

