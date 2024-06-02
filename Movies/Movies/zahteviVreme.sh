#/bin/bash -x -v

movies=(
    "Godfather" "Knight" "Schindler" "Fiction" "Fight" "Forrest" "Inception" "Matrix" "Goodfellas" "Samurai" "Se7en" "Pianist" "Terminator" "American" "Modern" "Psycho" "Gladiator" "City" "Departed" "Intouchables" "Whiplash" "Prestige" "Casablanca" "Paradiso" "Window" "Alien"
)

start_time=$(date +%s%3N)

for movie in "${movies[@]}"; do
    film="$movie"
    curl http://localhost:8084/$film > filmovi/$film &
done

wait

end_time=$(date +%s%3N)
elapsed_time=$((end_time - start_time))

echo "Gotovo"
echo "Elapsed time: $elapsed_time milliseconds"

