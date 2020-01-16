import React, { useState, useEffect } from 'react';
import { useParams } from 'react-router-dom';

export const Person = () => {
  let { id } = useParams();

  const [person, setPerson] = useState<any>(null);
  useEffect(() => {
    const fetchPerson = async () => {
      setPerson(await fetch('SitePeople/' + id).then(r => r.json()));
    };

    fetchPerson();
  }, [id]);

  if (!person) {
    return <div>loading</div>;
  }
  return (
    <div>
      Person {person.person.firstName} {person.person.lastName}
    </div>
  );
};
