# API Documentation

## Normal methods

```
puts(*lines)
	Outputs each line to the console.

print(*messages)
	Outputs each message to the console.

p(*lines)
	Outputs each line.inspect to the console.

gets
	Reads a line of user input from the console, including the trailing newline.

getc
	Reads a single key from the console and returns it as a character.

warn(*lines)
	Outputs each line to the console in yellow.

sleep([seconds = inf])
	Pauses the thread for the given number of seconds.

raise(exception_name) | raise(exception)
	Throws the given exception or a new exception with the given name.

throw(exception_name)
	Throws an exception with the given name that can be caught with catch.

catch(exception_name)
	Catches an exception thrown by throw with the given name.

lambda {}
	Returns a proc from the given block that can be called with call(*args).

loop {}
	Repeats the given block until a break or return statement is reached.

rand([max = 1.0]) | rand(range) | Random.range([max = 1.0]) | Random.range(range)
	If max is an integer, gets a random integer between 0 and (max - 1). If a float, gets a random float between 0 and max. If a range, gets a random integer within the range.

srand(seed) | Random.srand(seed)
	Sets the random number seed for rand() to the given seed integer and returns the previous seed.

exit | quit
	Throws an exception which cannot be caught.

eval(code)
	Evaluates the given code in the current context.

local_variables
	Returns an array containing the names of all current local variables as symbols.

global_variables
	Returns an array containing the names of all current global variables as symbols.

block_given?
	Returns true if a block is given to yield to.
```
```
object.inspect
	Returns a string describing the object in detail.

object.class
	Returns the class of the object.

object.to_s
	Returns a string representation of the object.

object.method(method_name)
	Returns a proc containing the method that can be called with call(*args).

object.constants
	Returns an array containing the names of all constants in the object as symbols.

object.object_id
	Returns an integer to uniquely identify the given object.

object.hash
	Returns an integer hash code that represents the object for Hash key lookups.

object.eql?(other)
	Returns true if the objects have the same hash.

object.methods
	Returns an array containing a symbol for the name of each method in a given object.

object.is_a?(class)
	Returns true if the object is an instance of the class or a class that inherits from it.

object.instance_of?(class)
	Returns true if the object is an instance of the class.

object.in?(array)
	Returns true if the array contains the item.

object.clone
	Returns a shallow copy of the object.

object.instance_methods
	Returns an array containing the names of all instance variables in the object as symbols.

instance.attr_reader(instance_variable_name)
	Creates an instance method called {the given name} which returns the given instance variable.

instance.attr_writer(instance_variable_name)
	Creates an instance method called {the given name}= which sets the instance variable to the given value.

instance.attr_accessor(instance_variable_name)
	Calls attr_writer and attr_reader.

instance.public
	Sets each method defined after it to public (the method can be called from anywhere).

instance.private
	Sets each method defined after it to private (the method can only be called from the current class/module).

instance.protected
	Sets each method defined after it to protected (the method can only be called from the current class/module or any class/module that inherits from it).
```
```
class.name
	Returns the name of the class as a string.

class.class_methods
	Returns an array containing the names of all class variables in the class as symbols.
```
```
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

string.length
	Returns the length of the string.

string.chomp | string.chomp!
	Returns the string with a single newline removed from the end.

string.strip | string.strip!
	Returns the string with all whitespace removed from the start and end.

string.lstrip | string.lstrip!
	Returns the string with all whitespace removed from the start.

string.rstrip | string.rstrip!
	Returns the string with all whitespace removed from the end.

string.squeeze | string.squeeze!
	Returns the string with all adjacent duplicate characters removed (e.g. "lollipop" becomes "lolipop").

string.chop | string.chop!
	Returns the string with a single character removed from the end.

string.chr
	Returns the first character in the string.

string.capitalize | string.capitalize!
	Returns the string in which the first letter is uppercase and the rest are lowercase (supports unicode).

string.upcase | string.upcase!
	Returns the string in uppercase.

string.downcase | string.downcase!
	Returns the string in lowercase.

string.sub(replace, with) | string.sub!(replace, with)
	Returns the string where the first instance of replace is replaced with with.

string.gsub(replace, with) | string.gsub!(replace, with)
	Returns the string where all instances of replace are replaced with with.

string.split([delimiter], [remove_empty_entries = true])
	Splits the string by the given delimiter or delimiters and returns an array of substrings. Delimiter defaults to all whitespace characters in the string.

string.eql?(other)
	Returns true if other is a string with the same contents.

string.include?(substring) | string.contain?(substring)
	Returns true if the string contains the substring.
```
```
integer.to_i
	Returns the integer.

integer.to_f
	Returns the integer as a float.

integer.times {|n|}
	Repeats the given block the integer number of times.

integer.clamp(min, max)
	Returns min if integer < min, max if integer > max or otherwise integer.

integer.round(decimal_places = 0)
	Returns the integer rounded to the given number of decimal places (which can be negative).

integer.floor, integer.ceil, integer.truncate
	Returns the integer.

integer.abs
	Returns the positive value of the integer.
```
```
Float::INFINITY
	A constant representing positive infinity.

float.to_i
	Returns the float as a truncated integer.

float.to_f
	Returns the float.

float.clamp(min, max)
	Returns min if float < min, max if float > max or otherwise float.

float.round(decimal_places = 0)
	Returns the float rounded to the given number of decimal places (which can be negative).

float.floor
	Returns the highest integer lower than the given float.

float.ceil
	Returns the lowest integer higher than the given float.

float.truncate
	Returns the float as an integer with the decimal places removed.

float.abs
	Returns the positive value of the float.
```
```
proc.call(*args)
	Calls the proc method with the given arguments.
```
```
range.min
	Gets the minimum value of the range.

range.max
	Gets the maximum value of the range after inclusive/exclusive is applied.

range.each {|i|}
	Repeats the given block for each index in the range.

range.reverse_each {|i|}
	Repeats the given block for each index in the range backwards.

range.length | range.count
	Returns the number of indexes in the range.

range.to_a
	Returns an array containing each index in the range.
```
```
array.push(item) | array.append(item)
	Adds the item to the end of the array and returns the array.
	
array.prepend(item)
	Inserts the item at the beginning of the array and returns the array.
	
array.pop
	Removes an item from the end of the array and returns the item.

array.insert(index, item) | array.insert(item)
	Inserts the item at the given array index, or adds it to the end of the array.

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

array.shuffle | array.shuffle!
	Returns the array in a randomised order.

array.min
	Returns the minimum item in the array using the < operator.

array.max
	Returns the maximum item in the array using the > operator.

array.sum
	Returns the sum of all items in the array of numbers.

array.each {|item, index|}
	Repeats the given block for each item in the array.

array.reverse_each {|item, index|}
	Repeats the given block for each item in the array backwards.

array.map {|item|} | array.map! {|item|}
	Returns a new array containing the values returned by the given block.

array.sort {|a, b|} | array.sort! {|item|}
	Sorts the array by the block's return value using a quick sort if there are more than 16 items, otherwise an insertion sort.
	The block should return -1 if a -> b, 0 if a and b are the same, and 1 if b -> a.
	The block defaults to "a <=> b" (ascending order).

array.include?(item) | array.contain?(item)
	Returns true if the array contains the item.

array.delete(item) | array.remove(item)
	Removes each item from the array that is equal to the item and returns the last item found.

array.delete_at(index) | array.remove_at(index)
	Removes the item at the index of the array if found and returns the item.

array.clear
	Removes every item from the array.

array.empty?
	Returns true if the array contains no items.

array.reverse | array.reverse!
	Returns the array in the opposite order.

array.join([separator = ''])
	Returns a string containing each item in the array as a string separated by the given separator.
```
```
Hash.new([default_value = nil])
	Returns a new hash, where if you try to index the hash with a key that doesn't exist, it returns default_value.

hash.has_key?(key)
	Returns true if the hash contains the key.

hash.has_value?(key)
	Returns true if the hash contains the value.

hash.keys
	Returns an array of the keys in the hash.

hash.values
	Returns an array of the values in the hash.

hash.delete(key) | hash.remove(key)
	Removes the key-value pair from the hash if found and returns the value or nil.

hash.clear
	Removes every key-value pair from the hash.

hash.each {|key, value|}
	Repeats the given block for each key-value pair in the hash.

hash.invert
	Returns a hash which contains the original hash's keys as values and values as keys.

hash.to_a
	Returns an array which contains an array of [key, value] for each key-value pair in the hash.

hash.to_hash
	Returns the hash.

hash.empty?
	Returns true if the hash contains no key-value pairs.
```
```
Math::PI
	A constant containing 17 digits of pi.

Math::E
	A constant containing 17 digits of e.

Math.sqrt(number)
	Returns the square root of the number.

Math.cbrt(number)
	Returns the cube root of the number.

Math.to_rad(degrees)
	Returns the degrees in radians.

Math.to_deg(radians)
	Returns the radians in degrees.

Math.lerp(a, b, t)
	Returns the linear value between a and b at the ratio t.

Math.abs(number)
	Returns the positive value of the number.

Math.sin, Math.cos, Math.tan, Math.asin, Math.acos, Math.atan, Math.atan2, Math.sinh, Math.cosh, Math.tanh, Math.asinh, Math.acosh,
Math.atanh, Math.exp, Math.log, Math.log10, Math.log2, Math.frexp, Math.ldexp, Math.hypot, Math.erf, Math.erfc, Math.gamma, Math.lgamma
	Various nerdy maths methods. Math.erf, Math.erfc, Math.gamma and Math.lgamma are only approximations.
```
```
Exception.new([message = ""])
	Returns a new exception with the given message.

exception.message
	Returns the exception's message.
```
```
Thread.new(*args) {}
	Runs the given block asynchronously and returns the thread.

thread.join
	Waits for the thread to finish.

thread.stop
	Stops the thread.
```
```
Parallel.each(array) {|n, i|}
	Repeats the given block for each item in the array in parallel.

Parallel.times(count) {|i|} | Parallel.times(range) {|i|}
	Repeats the given block for each index in the range in parallel.

Parallel.processor_count
	Returns the number of available logical processors.
```
```
Time.new | Time.new(year, [month = 0], [day = 0], [hour = 0], [minute = 0], [second = 0], [utc_offset = +0])
	Returns a time instance representing the local time or the given time.

Time.now
	Returns a time instance representing the local time.

Time.at(timestamp)
	Returns a time instance representing the time, timestamp seconds after the epoch (1970-01-01 00:00:00 +0).

time.to_i
	Returns the amount of seconds since the epoch as an integer.

time.to_f
	Returns the amount of seconds since the epoch as a float.
```
```
WeakRef.new(object)
	Creates a weak reference to the object which can be used exactly like it, but will not prevent it from being garbage collected.

weakref.weakref_alive?
	Returns false if the object has been garbage collected.
```
```
GC.start
	Initiates garbage collection for all generations.

GC.count([generation])
	Returns the number of times garbage has been collected for the given generation, or all generations combined.
```
```
__LINE__
	Returns the current script line as an integer.

EMBERS_VERSION
	A string containing the current version of Embers.

EMBERS_RELEASE_DATE
	A string containing the release date of the current version of Embers.

EMBERS_PLATFORM
	A string containing the local device's architecture followed by the operating system.

EMBERS_COPYRIGHT
	Me!

RUBY_COPYRIGHT
	Matz.
```

## Unsafe methods

```
system(code)
	(Windows only) Runs the given code in the command line and returns the output. If the command line waits for input, it will be stopped prematurely.
```
```
File.read(file_path)
	Reads the given file and returns its contents as a string.

File.write(file_path, text)
	Writes the text to the given file, overwriting it if it already exists.

File.append(file_path, text)
	Appends the text to the end of the given file.

File.delete(file_path)
	Deletes the given file if it exists.

File.exist?(file_path) | File.exists?(file_path)
	Returns true if a file exists with the given path.

File.absolute_path(file_path)
	Returns an absolute file path from the given relative file path.

File.absolute_path?(file_path)
	Returns true if the given file path is absolute (e.g. C://Documents/neko.jpg).
	
File.basename(file_path)
	Returns the filename and extension from the file path (e.g. C://Documents/neko.jpg becomes neko.jpg).
	
File.dirname(file_path)
	Returns the directory path from the file path (e.g. C://Documents/neko.jpg becomes C://Documents).
```
```
Net::HTTP.get(uri)
	Fetches and returns the content of the given URI as a Net::HTTP::HTTPResponse. Assumes https:// if the scheme is not included.

http_response.body
	Returns the HTML content of the HTTP Response.

http_response.code
	Returns the success code of the HTTP Response.
```