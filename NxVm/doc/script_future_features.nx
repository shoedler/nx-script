// -----------------------------------------
// Future features
// -----------------------------------------
cls NewFnSyntax {
  // Named Fns
  // No args decl 
  ctor { }
  fn a { }

  // Args decl
  ctor |a,b,c| { }
  fn a |a,b,c| { }

  fn method {
    
    // Anon / Lambda Fns
    // No args decl
    let a = -> { }
    // Args decl
    let a = a,b,c -> { }
  }
}


// TODO: Add support for pure objs
// TODO: Add support for sequences
// TODO: Add support for match
// TODO: Add support for tuples

// Seq
[] + "Strtoseq" = ["S", "t", "r", "t", "o", "s", "e", "q"]
[] // <- Seq
[1,2,3] + [4,5,6] = [1,2,3,4,5,6]
[1,2,3] + 4 = [1,2,3,4]

let u = [1,2,3] each(print) map(fn x -> x = x+1) tap(let x) map(fn x -> x = x+1)

// Match
let i = match [1,2,3] {
  [1,2,3] -> "In match context, we don't really compare references",
  [_,2,3] -> "Don't care about the first value, but it is followed by 2,3",
  [_,2,...] -> "Don't care about the first value, but it is followed by 2..",
  [@bool, @str, @obj, @fn, @num] -> "We can match on types",
  [...] -> {
    print "We can match on sequences of any length"
    ret "We can also return from match blocks"
  }
  some(fn x -> (x / 2) % 2 == 0) -> "Some element is even",
  some(1) -> "Some element is 1",
  every(1 or 2 or 3) -> "Every element is 1, 2 or 3",
  every(@num) -> "Every element is a number",
  some(@str) -> "Some element is a string",
  _ -> "Default case"
}

// Tuples
let t = (1,2,3) // <- Tuple
fn x -> q = ret (q, q+1) // <- Tuple