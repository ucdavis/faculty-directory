import React, { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';

export const People = (props: any) => {
  const [people, setPeople] = useState<any[]>([]);
  useEffect(() => {
    const getPeople = async () => {
      setPeople(await fetch('SitePeople').then(r => r.json()));
    };

    getPeople();
  }, []);

  // TOOD: use react table or something better for large lists
  const orderedPeople = people.sort((a: any, b: any) =>
    a.person.lastName.localeCompare(b.person.lastName)
  );

  return (
    <div className='content-wrapper'>
      <table className='table table-striped table-dark'>
        <thead>
          <tr>
            <th scope='col'>#</th>
            <th scope='col'>First</th>
            <th scope='col'>Last</th>
            <th scope='col'>Handle</th>
          </tr>
        </thead>
        <tbody>
          {orderedPeople.map(p => (
            <tr key={p.person.id}>
              <th scope='row'></th>
              <td>
                <Link to={'/People/' + p.person.id}>
                  {p.person.firstName} {p.person.lastName}
                </Link>
              </td>
              <td></td>
              <td></td>
              <td></td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
};
