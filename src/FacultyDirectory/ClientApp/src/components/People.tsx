import React, { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';

export const People = (props: any) => {
  const [people, setPeople] = useState<any[]>([]);
  useEffect(() => {
    console.log('use effect');
    getPeople();
  }, []);

  const getPeople = async () => {
    setPeople(await fetch('SitePeople').then(r => r.json()));
  };

  return (
    <div>
      <ul>
        {people.map(p => (
          <li key={p.person.id}>
            <Link to={'/SitePeople/' + p.person.id}>
            {p.person.firstName} {p.person.lastName}
            </Link>
          </li>
        ))}
      </ul>
    </div>
  );
};
