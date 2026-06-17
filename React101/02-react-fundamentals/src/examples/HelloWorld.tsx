/**
 * HelloWorld – The simplest React component.
 *
 * Key concepts:
 *  - A React component is just a function that returns JSX.
 *  - JSX looks like HTML but is compiled to React.createElement() calls.
 *  - Components must return a single root element (or a Fragment).
 *  - Component names start with an uppercase letter so React can
 *    distinguish them from regular HTML elements.
 */

export function HelloWorld() {
  // JSX is returned directly from the function.
  // Under the hood this compiles to: React.createElement('div', null, ...)
  return (
    <div className="example-container">
      <h1>Hello, World!</h1>
      <p>
        This is the simplest possible React component — a function that returns
        JSX.
      </p>

      <section className="code-note">
        <h3>What is JSX?</h3>
        <p>
          JSX is a syntax extension that lets you write HTML-like markup inside
          JavaScript. It gets compiled to regular JavaScript function calls.
        </p>
        <ul>
          <li>
            Use <code>className</code> instead of <code>class</code>
          </li>
          <li>
            Use <code>htmlFor</code> instead of <code>for</code>
          </li>
          <li>All tags must be closed, including self-closing tags</li>
          <li>
            Expressions go inside curly braces: <code>{'{ expression }'}</code>
          </li>
        </ul>
      </section>
    </div>
  )
}
