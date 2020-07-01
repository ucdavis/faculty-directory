import React, { useState, useEffect } from 'react';
import { useHistory } from 'react-router-dom';
import { Table, Button } from 'reactstrap';
import { IUser } from '../../models/IUsers';
import { UserInput } from './UserInput';

export const Users = () => {
  let history = useHistory();
  const [users, setUsers] = useState<IUser[]>([]);

  useEffect(() => {
    const getUsers = async () => {
      const results = await fetch('api/users');
      const response = await results;

      if (response.status == 403) {
        history.push('/error403');
      } else {
        const users = await results.json();
        setUsers(users);
      }
    };
    getUsers();
  }, []);

  const onSubmit = async (e: any, id: number) => {
    e.preventDefault();

    const headers = {
      Accept: 'application/json',
      'Content-Type': 'application/json'
    };

    await fetch('api/users/' + id, {
      method: 'DELETE',
      headers
    });

    setUsers(users.filter(user => user.id !== id));
  };

  return (
    <div>
      <UserInput users={users} onChange={setUsers} />

      <Table>
        <tbody>
          {users?.map(user => (
            <tr key={user.id}>
              <td>{user.username}</td>
              <td>
                <Button
                  size='lg'
                  color='danger'
                  onClick={e => {
                    onSubmit(e, user.id);
                  }}
                >
                  Delete
                </Button>{' '}
              </td>
            </tr>
          ))}
        </tbody>
      </Table>
    </div>
  );
};
