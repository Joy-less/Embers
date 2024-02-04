# Standard Library Documentation

```
puts(*lines)
	Outputs each line to the console.

print(*messages)
	Outputs each message to the console.

p(*lines)
	Outputs each line.inspect to the console.

warn(*lines)
	Outputs each line to the console.

gets
	Reads a line of user input from the console, including the trailing newline.

getc
	Reads a single character from the console.

sleep([seconds = inf])
	Pauses the thread for the given number of seconds.

loop {}
	Repeats the given block until a break or return statement is reached.

eval(code)
	Evaluates the given code in the current context.

raise() | raise(exception_name) | raise(exception)
	Throws the given exception or a new exception with the given name.

throw(identifier, [argument = nil])
	Throws an exception with the given identifier that can be caught with catch.

catch(identifier)
	Catches an exception thrown by throw with the given identifier.

lambda {}
	Returns a proc from the given block that can be called with call(*args).

exit([code = 0]) | quit([code = 0])
	Exits the application.

local_variables
	Returns an array containing the names of all current local variables as symbols.

global_variables
	Returns an array containing the names of all current global variables as symbols.

rand([max = 1.0]) | rand(range)
	If max is an integer, gets a random integer between 0 and (max - 1).
	If a float, gets a random float between 0 and max.
	If a range, gets a random integer within the range.

srand(seed)
	Sets the random number seed for rand() to the given seed integer and returns the previous seed.

public
	Sets each method defined after it to public (i.e. the method can be called from anywhere).

private
	Sets each method defined after it to private (i.e. the method can only be called from the current class/module).

protected
	Sets each method defined after it to protected (i.e. the method can only be called from the current class/module or any class/module that inherits from it).

attr_reader(instance_variable_name)
	Creates an instance method called instance_variable_name which returns the given instance variable.

attr_writer(instance_variable_name)
	Creates an instance method called instance_variable_name= which sets the instance variable to the given value.

attr_accessor(instance_variable_name)
	Calls attr_reader and attr_writer.
```
```
RUBY_VERSION
	A string containing the current version of Embers.

RUBY_RELEASE_DATE
	A string containing the release date of the current version of Embers.

RUBY_COPYRIGHT
	Me and Matz.

RUBY_PLATFORM
	A string containing the local device's architecture followed by the operating system.

DEBUG
	True if the .NET project is compiled in debug mode.

object.==(other)
	Returns true if the objects are the same reference.

object.!=(other)
	Returns the negated result of ==.

object.===(other)
	Returns the result of ==.

object.<=>(other)
	Returns 0 if the objects are the same reference, otherwise nil.

object.to_s
	Returns a string representation of the object.

object.inspect
	Returns a string describing the object.

object.hash
	Returns an integer hash code used for Hash key lookups.

object.class
	Returns the class of the object.

object.method(method_name)
	Returns a proc containing the method that can be called with proc.call(*args).

object.object_id
	Returns an integer that uniquely identifies the object reference.

object.is_a?(class)
	Returns true if the object is an instance of the class or a class that inherits from it.

object.instance_of?(class)
	Returns true if the object is an instance of the class.

object.in?(array)
	Returns true if the array contains the item.

object.eql?(other)
	Returns true if the objects have the same hash.

object.clone
	Returns a shallow copy of the object.

object.nil?
	Returns true if the object is nil.

object.methods
	Returns an array containing the names of all instance methods in the object as symbols.

object.instance_variables
	Returns an array containing the names of all instance variables in the object as symbols.

object.instance_methods
	Returns an array containing the names of all instance methods in the object as symbols.
```
```
module.name
	Returns the name of the class as a string.

module.methods
	Returns an array containing the names of all class methods in the object as symbols.

module.constants
	Returns an array containing the names of all constants in the class as symbols.

module.class_variables
	Returns an array containing the names of all class variables in the class as symbols.

module.class_methods
	Returns an array containing the names of all class methods in the class as symbols.
```
```
class.===(other)
	Returns true if the other object derives from the class.

class.superclass
	Returns the superclass of the class.

object.instance_variables
	Returns an array containing the names of all instance variables in the class as symbols.

object.instance_methods
	Returns an array containing the names of all instance methods in the class as symbols.
```
```
nil.inspect
	Returns "nil".
```
```
true.to_s
	Returns "true".

true.inspect
	Returns "true".
```
```
false.to_s
	Returns "false".

false.inspect
	Returns "true".
```
```
string.[index]
	Returns the substring at the index or index range of the string, otherwise nil.

string.[index]=(value)
	Sets the substring at the index or index range of the string.

string.+(other)
	Returns the string concatenated with the given string.

string.*(count)
	Returns the string repeated the given number of times.

string.==(other)
	Returns true if the other object is a string with the same contents.

string.<(other)
	Returns true if the string precedes the other string in the sort order.

string.<=(other)
	Returns true if the string precedes or equals the other string in the sort order.

string.>=(other)
	Returns true if the string succeeds or equals the other string in the sort order.

string.>(other)
	Returns true if the string succeeds the other string in the sort order.

string.<=>(other)
	Returns 0 if the string == the other object, otherwise -1, 0, or 1 if the other object is a string, otherwise nil.

string.to_sym
	Returns a (mortal) symbol representation of the string.

string.to_i
	Returns an integer representation of the string, otherwise nil.

string.to_f
	Returns a float representation of the string, otherwise nil.

string.to_a
	Returns an array containing each character of the string.

string.inspect
	Returns the string in quotes on one line.

string.length
	Returns the length of the string.

string.count([substring = null])
	Returns the number of times the substring occurs in the string, otherwise the length of the string.

string.chomp | string.chomp!
	Returns the string with a single newline removed from the end.

string.chop | string.chop!
	Returns the string with a single character removed from the end.

string.strip | string.strip!
	Returns the string with all whitespace removed from the start and end.

string.lstrip | string.lstrip!
	Returns the string with all whitespace removed from the start.

string.rstrip | string.rstrip!
	Returns the string with all whitespace removed from the end.

string.squeeze | string.squeeze!
	Returns the string with all adjacent duplicate characters removed (e.g. "lollipop" becomes "lolipop").

string.capitalize | string.capitalize!
	Returns the string in which the first letter is uppercase and the rest are lowercase.

string.upcase | string.upcase!
	Returns the string in uppercase.

string.downcase | string.downcase!
	Returns the string in lowercase.

string.sub(replace, with) | string.sub!(replace, with)
	Returns the string where the first instance of the substring is replaced.

string.gsub(replace, with) | string.gsub!(replace, with)
	Returns the string where all instances of the substring are replaced.

string.split([delimiter = " "], [limit = inf], [remove_empty_entries = true])
	Splits the string by the given delimiter or delimiters and returns an array of substrings.

string.chr
	Returns the first character in the string.

string.include?(substring) | string.contain?(substring)
	Returns true if the string contains the substring.

string.eql?(other)
	Returns true if other is a string with the same contents.
```
```
symbol.inspect
	Returns the symbol as a string.

symbol.length
	Returns the length of the symbol.
```
```
integer.+(other), integer.-(other), integer.*(other), integer./(other), integer.%(other), integer.**(other)
	Returns an operation performed on the integer and the other integer or float.

integer.+@, integer.-@
	Returns the unary operator performed on the integer.

integer.==(other)
	Returns true if the other object is an equivalent integer or float.

integer.<(other), integer.<=(other), integer.>=(other), integer.>(other), integer.<=>(other)
	Compares the integer and the other integer or float.

integer.to_i
	Returns the integer.

integer.to_f
	Returns the integer as a float.

integer.clamp(min, max)
	Returns min if integer < min, max if integer > max, otherwise integer.

integer.floor | integer.ceil | integer.round | integer.truncate
	Returns the integer.

integer.abs
	Returns the positive value of the integer.

integer.times {|n|}
	Repeats the given block from 0 to the integer, otherwise returns an enumerator.

integer.upto(limit) {|n|}
	Repeats the given block from the integer to the limit, otherwise returns an enumerator.

integer.downto(limit) {|n|}
	Repeats the given block from the limit to the integer, otherwise returns an enumerator.
```
```
Float::INFINITY
	A float that is positive infinity.
	
Float::NAN
	A float that is not a number.

float.+(other), float.-(other), float.*(other), float./(other), float.%(other), float.**(other)
	Returns an operation performed on the float and the other integer or float.

float.+@, float.-@
	Returns the unary operator performed on the float.

float.==(other)
	Returns true if the other object is an equivalent integer or float.

float.<(other), float.<=(other), float.>=(other), float.>(other), float.<=>(other)
	Compares the float and the other integer or float.

float.to_i
	Returns the float as a truncated integer.

float.to_f
	Returns the float.

float.clamp(min, max)
	Returns min if float < min, max if float > max, otherwise float.

float.floor
	Returns the highest integer lower than the given float.

float.ceil
	Returns the lowest integer higher than the given float.

float.round
	Returns the float rounded to the nearest integer.

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
array.[](index)
	Returns the item at the index or an array of items at the index range.

array.[]=(index, value)
	Sets the item at the index of the array.

array.*(count)
	Returns the array repeated the given number of times.

array.<<(item) | array.push(item) | array.append(item)
	Adds the item to the end of the array and returns the array.

array.==(other)
	Returns true if the other object is an array with equal contents.

array.to_s
	Returns a string containing each item.to_s of the array on a new line.

array.inspect
	Returns a string containing each item.inspect of the array in square brackets.

array.length
	Returns the number of items in the array.

array.count([item = nil])
	Returns the number of times the item appears in the array, otherwise the number of items in the array.
	
array.prepend(item)
	Inserts the item at the beginning of the array and returns the array.
	
array.pop
	Removes an item from the end of the array and returns the item.

array.insert(index, item) | array.insert(item)
	Inserts the item at the given array index, or adds it to the end of the array.

array.delete(item) | array.remove(item)
	Removes each item from the array that is equal to the item and returns the last item found.

array.delete_at(index) | array.remove_at(index)
	Removes the item at the index of the array if found and returns the item.

array.uniq | array.uniq!
	Returns the array with duplicates removed (according to their hashes).

array.first
	Returns the first item in the array.

array.last
	Returns the last item in the array.

array.forty_two
	Returns the forty-second item in the array.

array.sample
	Returns a random item in the array.

array.min
	Returns the minimum item in the array using the < operator.

array.max
	Returns the maximum item in the array using the > operator.

array.sum
	Returns the sum of all items in the array of numbers.

array.each {|item, index|}
	Repeats the given block for each item in the array, otherwise returns an enumerator.

array.reverse_each {|item, index|}
	Repeats the given block for each item in the array backwards, otherwise returns an enumerator.

array.shuffle | array.shuffle!
	Returns the array in a randomised order.

array.sort {|a, b|} | array.sort! {|item|}
	Sorts the array by the block's return value.
	The block should return -1 if a comes before b, 0 if a and b are the same, and 1 if b comes before a.
	The block defaults to "a <=> b" (ascending order).

array.map {|item|} | array.map! {|item|}
	Returns a new array containing the values returned by the given block.

array.reverse | array.reverse!
	Returns the array in the opposite order.

array.join([separator = ''])
	Returns a string containing each item in the array as a string separated by the given separator.

array.clear
	Removes every item from the array.

array.include?(item) | array.contain?(item)
	Returns true if the array contains the item.

array.empty?
	Returns true if the array contains no items.
```
```
Hash.new([default_value = nil])
	Returns a new hash, which returns default_value if you try to access a key that doesn't exist.

hash.[](key)
	Gets the value at the given key.

hash.[]=(key, value)
	Sets the value at the given key.

hash.==(other)
	Returns true if the other object is a hash with equal entries.

hash.to_s
	Returns a string containing each item.inspect of the hash in curly brackets.

hash.to_a
	Returns an array containing each [key, value] in the hash.

hash.inspect
	Returns a string containing each item.inspect of the hash in curly brackets.

hash.length | hash.count
	Returns the number of entries in the hash.

hash.has_key?(key)
	Returns true if the hash contains the key.

hash.has_value?(key)
	Returns true if the hash contains the value.

hash.keys
	Returns an array of keys in the hash.

hash.values
	Returns an array of values in the hash.

hash.delete(key) | hash.remove(key)
	Removes the entry pair from the hash if found and returns the value or nil.

hash.clear
	Removes every entry from the hash.

hash.each {|key, value|}
	Repeats the given block for entry in the hash.

hash.reverse_each {|key, value|}
	Repeats the given block for entry in the hash in reverse.

hash.invert
	Returns a hash which contains the keys and values swapped.

hash.empty?
	Returns true if the hash contains no key-value pairs.
```
```
Time.new | Time.new([year = 0], [month = 0], [day = 0], [hour = 0], [minute = 0], [second = 0], [utc_offset = +0])
	Returns a time instance representing the given time.

Time.now
	Returns a time instance representing the local time.

Time.at(timestamp)
	Returns a time instance representing the time, timestamp seconds after the epoch (1970-01-01 00:00:00 +0).

time.to_s
	Returns a string containing the time in Japanese format.

time.to_i
	Returns the number of seconds since the epoch as an integer.

time.to_f
	Returns the number of seconds since the epoch as a float.
```
```
range.min
	Gets the minimum value of the range.

range.max
	Gets the maximum value of the range.

range.exclude_end?
	Returns true if range.max has been reduced by one.

range.each {|i|}
	Repeats the given block for each index in the range, otherwise returns an enumerator.

range.reverse_each {|i|}
	Repeats the given block for each index in the range backwards, otherwise returns an enumerator.

range.to_a
	Returns an array containing each index in the range.

range.length | range.count
	Returns the number of indexes in the range.
```
```
enumerator.each {|item|}
	Repeats the given block for each item in the enumerator, otherwise returns itself.

enumerator.step(interval) {|item|}
	Repeats the given block for each item in the enumerator, skipping the given number of items after each item, otherwise returns an enumerator.

enumerator.next
	Moves the enumerator to the next item and returns it.

enumerator.peek
	Returns the next item in the enumerator.
```
```
Exception.new([message = nil])
	Returns a new exception with the given message.

exception.to_s
	Returns "exception".

exception.inspect
	Returns the exception's type and message.

exception.message
	Returns the exception's message.

exception.backtrace
	Returns the exception's stack trace.
```
```
WeakRef.new(object)
	Returns a weak reference to the object. It can be used as if it was the object, but will not prevent it from being garbage collected.

weakref.to_s
	Returns "weakref".

weakref.inspect
	Returns a string containing the weakref and the weakref's target.inspect.

weakref.weakref_alive?
	Returns false if the object has been garbage collected.

weakref.method_missing(method_name, *arguments) {}
	Calls the method on the object if it's still alive, otherwise throws an error.
```
```
Thread.new(*args) {}
	Runs the given block asynchronously and returns the thread.

thread.stop
	Stops the thread.

thread.join
	Waits for the thread to finish.
```
```
Math::PI
	A float containing pi (3.14...).

Math::E
	A float containing e (2.71...).

Math::TAU
	A float containing tau (6.28...)

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
Math.atanh, Math.exp, Math.log, Math.log10, Math.log2, Math.frexp, Math.ldexp, Math.hypot
	Methods for nerds.
```
```
GC.start([max_generation = nil])
	Initiates garbage collection for all generations up to the given generation, otherwise for all generations.

GC.count([generation = nil])
	Returns the number of times garbage has been collected for the given generation, or all generations combined.
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

File.exist?(file_path)
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