import React, { useState } from 'react';
import { Button, Input, InputGroup, InputGroupAddon } from 'reactstrap';
import { IUser } from '../../models/IUsers';

interface IProps {
  users: IUser[];
  onChange: Function;
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

    const response = await fetch('api/users', {
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
    <InputGroup>
      <Input
        placeholder='Add a user'
        value={username}
        onChange={handleUsername}
      />
      <InputGroupAddon addonType='append'>
        <Button color='primary' onClick={onSubmit}>
          Submit
        </Button>
      </InputGroupAddon>
    </InputGroup>
  );
};
