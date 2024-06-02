#/bin/bash -x -v
movies=(
	 "Godfather" "Knight" "Schindler" "Fiction" "Fight" "Forrest" "Inception" "Matrix" "Goodfellas" "Samurai" "Se7en" "Pianist" "Terminator" "American" "Modern" "Psycho" "Gladiator" "City" "Departed" "Intouchables" "Whiplash" "Prestige" "Casablanca" "Paradiso" "Window" "Alien")

for movie in "${movies[@]}"; do
	#Bez zapisivanja u fajl
	#curl http://localhost:8083/$movie
	#sa zapisivanjem u fajlu
	film="$movie"
	curl http://localhost:8084/$film> filmovi/$film &
#	echo $film
    	#echo "Movie: $movie"
done

echo "Gotovo"
