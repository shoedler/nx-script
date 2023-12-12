// Without emitting OP_INVOKE
cls Foo {
  ctor -> {
    fn bar -> {
      print "not a method"
    }

    this.field = bar
  }
}

let foo = Foo()
foo.field() // Goes via call_value()
print foo.field
