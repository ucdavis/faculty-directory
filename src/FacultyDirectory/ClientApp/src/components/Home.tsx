import React, { Component } from 'react';

export class Home extends Component {
  static displayName = Home.name;

  render() {
    return (
      <div>
        <h1>Hello, world!</h1>
        <p>Welcome to your new single-page application, built with:</p>
        <ul>
          <li>
            <a href='https://get.asp.net/'>ASP.NET Core</a> and{' '}
            <a href='https://msdn.microsoft.com/en-us/library/67ef8sbd.aspx'>
              C#
            </a>{' '}
            for cross-platform server-side code
          </li>
          <li>
            <a href='https://facebook.github.io/react/'>React</a> for
            client-side code
          </li>
          <li>
            <a href='http://getbootstrap.com/'>Bootstrap</a> for layout and
            styling
          </li>
        </ul>
        <p>To help you get started, we have also set up:</p>
        <ul>
          <li>
            <strong>Client-side navigation</strong>. For example, click{' '}
            <em>Counter</em> then <em>Back</em> to return here.
          </li>
          <li>
            <strong>Development server integration</strong>. In development
            mode, the development server from <code>create-react-app</code> runs
            in the background automatically, so your client-side resources are
            dynamically built on demand and the page refreshes when you modify
            any file.
          </li>
          <li>
            <strong>Efficient production builds</strong>. In production mode,
            development-time features are disabled, and your{' '}
            <code>dotnet publish</code> configuration produces minified,
            efficiently bundled JavaScript files.
          </li>
        </ul>
        <p>
          The <code>ClientApp</code> subdirectory is a standard React
          application based on the <code>create-react-app</code> template. If
          you open a command prompt in that directory, you can run{' '}
          <code>npm</code> commands such as <code>npm test</code> or{' '}
          <code>npm install</code>.
        </p>

        <form className='dark-form'>
          <div className='form-group'>
            <label>Example label</label>
            <input
              type='text'
              className='form-control'
              id='formGroupExampleInput'
              placeholder='Example input placeholder'
            />
          </div>
          <div className='form-group'>
            <label>Another label</label>
            <input
              type='text'
              className='form-control'
              id='formGroupExampleInput2'
              placeholder='Another input placeholder'
            />
          </div>
          <div className='form-group'>
            <label>Example textarea</label>
            <textarea
              className='form-control'
              id='exampleFormControlTextarea1'
              rows={3}
            ></textarea>
          </div>
        </form>
        <form className='dark-form'>
          <div className='form-group'>
            <label>Email address</label>
            <input
              type='email'
              className='form-control'
              id='exampleFormControlInput1'
              placeholder='name@example.com'
            />
          </div>
          <div className='form-group'>
            <label>Example select</label>
            <select className='form-control' id='exampleFormControlSelect1'>
              <option>1</option>
              <option>2</option>
              <option>3</option>
              <option>4</option>
              <option>5</option>
            </select>
          </div>
          <div className='form-group'>
            <label>Example multiple select</label>
            <select
              multiple
              className='form-control'
              id='exampleFormControlSelect2'
            >
              <option>1</option>
              <option>2</option>
              <option>3</option>
              <option>4</option>
              <option>5</option>
            </select>
          </div>
          <div className='form-group'>
            <label>Example textarea</label>
            <textarea
              className='form-control'
              id='exampleFormControlTextarea1'
              rows={3}
            ></textarea>
          </div>
        </form>
        <form className='dark-form'>
          <select className='form-control form-control-lg'>
            <option>Large select</option>
          </select>
          <select className='form-control'>
            <option>Default select</option>
          </select>
          <select className='form-control form-control-sm'>
            <option>Small select</option>
          </select>
          <label>Disabled form input</label>
          <input
            className='form-control'
            type='text'
            placeholder='Readonly input here...'
            value="I have something to say"
            readOnly
          />
        </form>
      </div>
    );
  }
}
