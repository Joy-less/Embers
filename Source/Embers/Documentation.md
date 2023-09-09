# API Documentation

## Normal methods

```
puts(*lines)
	Outputs each line to the console.

print(*messages)
	Outputs each message to the console.

p(*lines)
	Inspects and outputs each line to the console.

gets
	Reads a line of user input from the console, including the trailing newline.

getc
	Reads a single key from the console and returns it as a character.

warn(*lines)
	Outputs each line to the console in yellow.

sleep([seconds = inf])
	Pauses the thread for the given number of seconds.

raise(error_name)
	Throws an error with the given name.

throw(error_name)
	Throws an error with the given name that can be caught with catch.

catch(error_name)
	Catches an error thrown by throw with the given name.

lambda
	Returns a proc from the given block that can be called with call(*args).

loop
	Repeats the given block until a break or return statement is reached.

rand([max = 1.0])
	If max is an integer, gets a random integer between 0 and max-1. If max is a float, gets a random float between 0 and max.

srand(seed)
	Sets the random number seed for rand() to the given seed integer and returns the previous seed.

exit
quit
	Throws an error which cannot be caught.

eval(code)
	Evaluates the given code in the current context.

object.to_s
	Returns a string representation of the object.

string.to_str
	Returns the string.

string.to_i
	Returns an integer representation of the string, otherwise 0.

string.to_f
	Returns a float representation of the string, otherwise 0.0.

string.to_sym
	Returns a symbol representation of the string.

string.to_a
	Returns an array containing each character of the string.

string.chomp
	Returns the string with a single newline removed from the end.

string.strip
	Returns the string with all whitespace removed from the start and end.

string.lstrip
	Returns the string with all whitespace removed from the start.

string.rstrip
	Returns the string with all whitespace removed from the end.

string.squeeze
	Returns the string with all adjacent duplicate characters removed (e.g. "lollipop" becomes "lolipop").

string.chop
	Returns the string with a single character removed from the end.

string.chr
	Returns the first character of the string.

string.capitalize
	Returns the string in which the first letter is uppercase and the rest are lowercase (supports unicode).

string.upcase
	Returns the string in uppercase.

string.lowercase
	Returns the string in lowercase.

string.sub(replace, with)
	Returns the string where the first instance of replace is replaced with with.

string.gsub(replace, with)
	Returns the string where all instances of replace are replaced with with.

integer.to_i
	Returns the integer.

integer.to_f
	Returns the integer as a float.

integer.times |n|
	Repeats the given block the integer number of times.

float.to_i
	Returns the float as a truncated integer.

float.to_f
	Returns the float.

proc.call(*args)
	Calls the proc method with the given arguments.

array.length
	Returns the number of items in the array.

array.count([item])
	If item is given, returns the number of times it appears in the array. Otherwise, returns the number of items in the array.

array.first
	Returns the first item in the array.

array.last
	Returns the last item in the array.

array.forty_two
	Returns the forty-second item in the array.

array.sample
	Returns a random item in the array.

array.insert(index, item) | array.insert(item)
	Inserts the item into the given index in the array, or adds it to the end of the array.

array.each |item, index|
	Repeats the given block for each item in the array.

array.reverse_each |item, index|
	Repeats the given block for each item in the array backwards.

array.map |item|
	Returns a new array containing the values returned by the given block.

array.contains?(item)
	Returns true if the array contains the item.

array.include?(item)
	Returns true if the array contains the item.

hash.has_key?(key)
	Returns true if the hash contains the key.

hash.has_value?(key)
	Returns true if the hash contains the value.

hash.keys
	Returns an array of the keys in the hash.

hash.values
	Returns an array of the values in the hash.

hash.invert
	Returns a hash which contains the original hash's keys as values and values as keys.

hash.to_a
	Returns an array which contains an array of [key, value] for each key-value pair in the hash.

hash.to_hash
	Returns the hash.
```

## Unsafe methods

```
system(code)
	(Windows only) Runs the given code in the command line and returns the output. If the command line waits for input, it will be stopped prematurely.

File.read(file_path)
	Reads the given file and returns its contents as a string.

File.write(file_path, text)
	Writes the text to the given file.
```