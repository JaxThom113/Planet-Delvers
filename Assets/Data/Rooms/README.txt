This folder contains room data the world generation algorithm uses

They are organized by length x height format.

i.e. /Rooms/1/1 => length 1 x height 2 room, and /Rooms/3/4 => length 3 x height 4 room

Inside a folder for a particular room layout, there are csv files named 1.csv -> x.csv
where x is the number of cells the room occupies. They are read bottom left to top right,
going left to right for each row.

i.e. 2x3_1 would go 1.csv -> 2.csv for the first row, 3.csv -> 4.csv for the second row, 
and 5.csv -> 6.csv for the third and final row
