import React, { useState, useEffect } from 'react';
import { useParams } from 'react-router-dom';

export const Person = () => {
  let { id } = useParams();
  
  const [personBundle, setPersonBundle] = useState<any>(null);
  useEffect(() => {
    const fetchPerson = async () => {
      setPersonBundle(await fetch('SitePeople/' + id).then(r => r.json()));
    };

    fetchPerson();
  }, [id]);

  if (!personBundle) {
    return <div>loading</div>;
  }

  const { person, sitePerson } = personBundle;
  return (
    <div>
      Person {person.firstName} {person.lastName}
      <p>Site info: {sitePerson ? 'yup' : 'nope'}</p>
    </div>
  );
};
