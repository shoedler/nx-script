cls CoffeeMaker {
  ctor = coffee -> {
    this.coffee = coffee
  }

  fn brew -> {
    print "Enjoy your cup of " + this.coffee

    // No reusing the grounds!
    this.coffee = nil
  }
}

let maker = CoffeeMaker("coffee and chicory")
maker.brew()
maker.brew() // Error Operands must be two numbers or two strings.
