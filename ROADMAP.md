# Roadmap

Tasks are sorted in order of least, to most difficult to implement.

> [!TIP]
> Checkout https://github.com/TodePond/GulfOfMexico for more ingenious ideas.

- [ ] **Employee Wisdom** - Add a `quote()` function that randomly selects inspirational or humorous quotes from your employees. Store these in a JSON file that can be easily updated when someone says something quotable during meetings.
- [ ] **Constants** - Add a `const` keyword that allows you to define immutable constants in the script.
- [ ] **Nexplore Info** - Add add a `nexplore()` function that returns an object containing all addresses of nexplore offices, current share value and current employee-count.
- [ ] **Maybe Operator** - Add a `maybe` keyword that executes code with a configurable probability:
  ```
  maybe (0.5) {
    // This code has a 50% chance of running
    buy_lottery_ticket();
  }
  ```
- [ ] **Within Operator** - Add `within` keyword which forces execution time limits on a block of code:
  ```
  within (200ms) {
    heavy_computation();
  } else {
    use_backup_result();
  }
  ```
- [ ] **Universal global constants** - Add an `axiom` or `decree` keyword, that allows you to define global constants that are accessible from any script. They can never go out of scope - not even when the script ends. They are immediately available in any script that is run, on any machine.
