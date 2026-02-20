import { useState } from 'react';
import { IUser } from '../../models/IUsers';

interface IProps {
  users: IUser[];
  onChange: (users: IUser[]) => void;
}

export const UserInput = (props: IProps) => {
  const [username, setUsername] = useState('');

  const handleUsername = (e: any) => {
    setUsername(e.target.value);
  };

  const onSubmit = async (e: any) => {
    e.preventDefault();

    const headers = {
      Accept: 'application/json',
      'Content-Type': 'application/json'
    };

    const userData = JSON.stringify({ username: username });

    const response = await fetch('/api/users', {
      method: 'POST',
      headers,
      body: userData
    });

    if (response.status == 400) {
      alert('User already exists');
    } else {
      const newUser = await response.json();
      props.onChange([...props.users, newUser]);
    }

    setUsername('');
  };

  return (
    <div className='input-group'>
      <input
        className='form-control'
        placeholder='Add a user'
        value={username}
        onChange={handleUsername}
      />
      <button className='btn btn-primary' onClick={onSubmit}>
        Submit
      </button>
    </div>
  );
};
