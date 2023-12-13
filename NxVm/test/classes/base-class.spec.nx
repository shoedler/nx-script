cls A {
  fn method -> {
    print "A"
  }
}

cls B : A {
  fn method -> {
    let closure = base.method
    closure() // Prints "A"
  }
}

let b = B()
b.method()