

interface IProps {
  hasActivity: boolean;
  children: any;
}

export const ActivityWrapper = (props: IProps) => {
  return (
    <div className={props.hasActivity ? 'custom-active-wrapper' : ''}>
      {props.children}
    </div>
  );
};
