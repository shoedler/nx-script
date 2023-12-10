let fib = fn n ->
  if n <= 1
    ret n;
  else
    ret (fib(n-1) + fib(n-2));

// With native functions. Here, timed with clock()
let start = clock()
print "Let's calculate fib(10)!"
let result = fib(10)
print result
print clock() - start